// Package service — бизнес-логика каркаса: приём реплея дейлика, предотсев по заголовку, ре-симуляция, запись в таблицу.
package service

import (
	"context"
	"fmt"
	"strconv"
	"time"

	"github.com/ArtemChikolaev/Glimmerfall/server/internal/apperr"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/daily"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/replay"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/store"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/validator"
)

// Verifier — валидатор реплеев дейлика (интерфейс ради тестов без .NET).
type Verifier interface {
	VerifyDaily(ctx context.Context, date string, replay []byte) (validator.Verdict, error)
}

// Result — ответ на приём реплея.
type Result struct {
	Board   string            `json:"board"`
	Rank    int               `json:"rank"`
	Verdict validator.Verdict `json:"verdict"`
}

// Replays принимает реплеи дейлика. Версию баланса сервер не знает: чужую отвергает сам валидатор (SimVersion.Balance).
type Replays struct {
	Verifier Verifier
	Boards   *store.Leaderboards
	Now      func() time.Time
}

// Today — параметры сегодняшнего дейлика.
func (s *Replays) Today() daily.Daily {
	return daily.For(s.Now())
}

// SubmitDaily: предотсев (формат, сид дня), затем ре-симуляция валидатором — он же сверяет версию баланса
// и канонический конфиг дня (sim/DailyRules.cs). Очки — только из вердикта, не от клиента.
func (s *Replays) SubmitDaily(ctx context.Context, player string, data []byte) (Result, error) {
	if player == "" {
		return Result{}, apperr.BadRequest("player_required", "player is required")
	}
	header, err := replay.ParseHeader(data)
	if err != nil {
		return Result{}, apperr.BadRequest("bad_replay", err.Error())
	}
	today := s.Today()
	if header.Seed != today.Seed {
		return Result{}, apperr.Unprocessable("not_today_daily",
			fmt.Sprintf("seed %d is not today's daily %s", header.Seed, today.SeedText))
	}
	verdict, err := s.Verifier.VerifyDaily(ctx, today.Date, data)
	if err != nil {
		return Result{}, apperr.Internal("validator failed").Wrap(err)
	}
	if !verdict.OK {
		return Result{Verdict: verdict}, apperr.Unprocessable("replay_rejected", verdict.Message)
	}
	if verdict.Seed != strconv.FormatUint(today.Seed, 10) {
		return Result{Verdict: verdict}, apperr.Unprocessable("replay_rejected", "validator saw a different seed")
	}
	board := "daily-" + today.Date
	rank := s.Boards.Submit(board, store.Entry{
		Player:   player,
		Score:    verdict.Score,
		Ticks:    verdict.Ticks,
		Outcome:  verdict.Outcome,
		Digest:   verdict.Digest,
		Accepted: s.Now(),
	})
	return Result{Board: board, Rank: rank, Verdict: verdict}, nil
}
