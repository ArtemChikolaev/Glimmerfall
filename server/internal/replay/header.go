// Package replay читает заголовок текстового реплея (sim/Replays/Replay.cs) — ровно то, что нужно серверу, чтобы
// отбросить чужой реплей до дорогой ре-симуляции: формат, версию баланса и сид. Остальное проверяет валидатор.
package replay

import (
	"bufio"
	"bytes"
	"fmt"
	"strconv"
	"strings"
)

// Header — поля заголовка для предотсева.
type Header struct {
	Balance string
	Seed    uint64
}

// ParseHeader разбирает строки «ключ значение» до лога ввода.
func ParseHeader(data []byte) (Header, error) {
	var h Header
	sawMagic, sawSeed := false, false
	scanner := bufio.NewScanner(bytes.NewReader(data))
	scanner.Buffer(make([]byte, 64*1024), len(data)+1)
	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}
		key, value, _ := strings.Cut(line, " ")
		switch key {
		case "glimmerfall-replay":
			if value != "1" {
				return h, fmt.Errorf("unsupported replay format %q", value)
			}
			sawMagic = true
		case "balance":
			h.Balance = value
		case "seed":
			seed, err := strconv.ParseUint(value, 10, 64)
			if err != nil {
				return h, fmt.Errorf("bad seed: %w", err)
			}
			h.Seed = seed
			sawSeed = true
		case "inputs":
			if !sawMagic || !sawSeed || h.Balance == "" {
				return h, fmt.Errorf("replay header is incomplete")
			}
			return h, nil
		}
	}
	if err := scanner.Err(); err != nil {
		return h, err
	}
	return h, fmt.Errorf("replay has no input log")
}
