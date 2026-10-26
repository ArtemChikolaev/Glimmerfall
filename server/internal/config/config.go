// Package config читает конфигурацию сервера из env: секреты и пути — не в коде (CLAUDE.md, раздел «IP и ассеты»).
// Значения по умолчанию — для локального запуска рядом с собранным sim.cli.
package config

import (
	"os"
	"strconv"
	"strings"
	"time"
)

// Config — настройки HTTP-сервера и валидатора реплеев.
type Config struct {
	Env  string // dev|prod
	Port string
	// ValidatorCmd — команда валидатора: читает реплей из stdin и печатает JSON-вердикт.
	// Локально — собранный glimmerfall-sim, в проде — контейнер validator/ (docker run -i …).
	ValidatorCmd     []string
	ValidatorTimeout time.Duration
	// MaxReplayBytes — потолок тела реплея: 30 минут ручного ввода с аналоговым стиком укладываются с запасом.
	MaxReplayBytes int64
	ReadTimeout    time.Duration
	WriteTimeout   time.Duration
	IdleTimeout    time.Duration
}

// Load собирает конфиг из env с дефолтами.
func Load() Config {
	return Config{
		Env:              env("APP_ENV", "dev"),
		Port:             env("PORT", "8080"),
		ValidatorCmd:     strings.Fields(env("VALIDATOR_CMD", "glimmerfall-sim verify --json -")),
		ValidatorTimeout: time.Duration(envInt("VALIDATOR_TIMEOUT_SECONDS", 60)) * time.Second,
		MaxReplayBytes:   int64(envInt("MAX_REPLAY_BYTES", 4<<20)),
		ReadTimeout:      10 * time.Second,
		WriteTimeout:     90 * time.Second,
		IdleTimeout:      60 * time.Second,
	}
}

func env(key, fallback string) string {
	if value, ok := os.LookupEnv(key); ok && value != "" {
		return value
	}
	return fallback
}

func envInt(key string, fallback int) int {
	value, err := strconv.Atoi(env(key, ""))
	if err != nil {
		return fallback
	}
	return value
}
