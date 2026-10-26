// Command api — каркас сервера Glimmerfall (BACKLOG G1.7): дейлик, приём реплеев с ре-симуляцией .NET-валидатором,
// таблица рекордов в памяти. Postgres, вход через Steam и trusted-запись в лидерборды Steam — следующие шаги.
//
//	VALIDATOR_CMD="../sim.cli/bin/Release/net10.0/glimmerfall-sim verify --json -" go run ./cmd/api
package main

import (
	"context"
	"errors"
	"log"
	"net/http"
	"os/signal"
	"syscall"
	"time"

	"github.com/ArtemChikolaev/Glimmerfall/server/internal/config"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/service"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/store"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/transport"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/validator"
)

func main() {
	if err := run(); err != nil {
		log.Fatalf("[server] %v", err)
	}
}

func run() error {
	cfg := config.Load()
	ctx, stop := signal.NotifyContext(context.Background(), syscall.SIGINT, syscall.SIGTERM)
	defer stop()

	boards := store.NewLeaderboards()
	replays := &service.Replays{
		Verifier: validator.Runner{Cmd: cfg.ValidatorCmd, Timeout: cfg.ValidatorTimeout},
		Boards:   boards,
		Now:      time.Now,
	}
	srv := &http.Server{
		Addr:         ":" + cfg.Port,
		Handler:      transport.NewServer(replays, boards, cfg.MaxReplayBytes).Routes(),
		ReadTimeout:  cfg.ReadTimeout,
		WriteTimeout: cfg.WriteTimeout,
		IdleTimeout:  cfg.IdleTimeout,
	}
	errs := make(chan error, 1)
	go func() {
		log.Printf("[server] listening on :%s (env %s, validator %v)", cfg.Port, cfg.Env, cfg.ValidatorCmd)
		errs <- srv.ListenAndServe()
	}()
	select {
	case err := <-errs:
		if !errors.Is(err, http.ErrServerClosed) {
			return err
		}
		return nil
	case <-ctx.Done():
	}
	shutdown, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()
	return srv.Shutdown(shutdown)
}
