// Package validator запускает .NET-валидатор — тот же Sim.dll, что у игры (ADR 0002): реплей уходит в stdin,
// вердикт приходит одной JSON-строкой (`glimmerfall-sim verify --json -`). Сервер не верит очкам клиента.
package validator

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"fmt"
	"os/exec"
	"strings"
	"time"
)

// Verdict — контракт с sim.cli/ReplayCommands.cs (класс Verdict).
type Verdict struct {
	OK      bool   `json:"ok"`
	Message string `json:"message"`
	Balance string `json:"balance"`
	Seed    string `json:"seed"`
	Steps   int    `json:"steps"`
	Ticks   int    `json:"ticks"`
	Outcome string `json:"outcome"`
	Digest  string `json:"digest"`
	Score   int    `json:"score"`
}

// Runner — внешний процесс валидатора с таймаутом.
type Runner struct {
	Cmd     []string
	Timeout time.Duration
}

// VerifyDaily ре-симулирует реплей дейлика даты (валидатор сверяет сид и канонический конфиг дня).
// Ошибка — сбой самого валидатора; отвергнутый реплей — Verdict.OK == false.
func (r Runner) VerifyDaily(ctx context.Context, date string, replay []byte) (Verdict, error) {
	if len(r.Cmd) == 0 {
		return Verdict{}, errors.New("validator command is empty")
	}
	ctx, cancel := context.WithTimeout(ctx, r.Timeout)
	defer cancel()
	args := append(append([]string(nil), r.Cmd[1:]...), "--daily", date)
	cmd := exec.CommandContext(ctx, r.Cmd[0], args...)
	cmd.Stdin = bytes.NewReader(replay)
	var stdout, stderr bytes.Buffer
	cmd.Stdout = &stdout
	cmd.Stderr = &stderr
	runErr := cmd.Run()
	if ctx.Err() != nil {
		return Verdict{}, fmt.Errorf("validator timed out after %s", r.Timeout)
	}
	line := strings.TrimSpace(stdout.String())
	if line == "" {
		return Verdict{}, fmt.Errorf("validator produced no verdict: %v: %s", runErr, strings.TrimSpace(stderr.String()))
	}
	var verdict Verdict
	if err := json.Unmarshal([]byte(line), &verdict); err != nil {
		return Verdict{}, fmt.Errorf("validator verdict is not JSON: %w", err)
	}
	return verdict, nil
}
