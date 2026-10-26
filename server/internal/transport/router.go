// Package transport — HTTP-слой (образец aegis-draft: тонкие хендлеры, логика в service, контракт ошибок apperr).
// Маршруты — стандартный http.ServeMux с методами в шаблонах (Go 1.22+): в каркасе внешние зависимости не нужны.
package transport

import (
	"context"
	"encoding/json"
	"errors"
	"io"
	"log"
	"net/http"

	"github.com/ArtemChikolaev/Glimmerfall/server/internal/apperr"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/daily"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/service"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/store"
)

// Replays — зависимость хендлеров (интерфейс ради тестов).
type Replays interface {
	Today() daily.Daily
	SubmitDaily(ctx context.Context, player string, data []byte) (service.Result, error)
}

// Server держит зависимости хендлеров.
type Server struct {
	replays        Replays
	boards         *store.Leaderboards
	maxReplayBytes int64
}

func NewServer(replays Replays, boards *store.Leaderboards, maxReplayBytes int64) *Server {
	return &Server{replays: replays, boards: boards, maxReplayBytes: maxReplayBytes}
}

// Routes — карта маршрутов каркаса.
//
//	GET  /healthz                      liveness
//	GET  /v1/daily                     дата и сид сегодняшнего дейлика (правила — в Sim.dll клиента)
//	POST /v1/daily/replays?player=ID   реплей дейлика → ре-симуляция → место в таблице
//	GET  /v1/daily/leaderboard         топ-20 сегодняшнего дейлика
func (s *Server) Routes() http.Handler {
	mux := http.NewServeMux()
	mux.HandleFunc("GET /healthz", func(w http.ResponseWriter, _ *http.Request) {
		writeJSON(w, http.StatusOK, map[string]string{"status": "ok"})
	})
	mux.HandleFunc("GET /v1/daily", func(w http.ResponseWriter, _ *http.Request) {
		writeJSON(w, http.StatusOK, s.replays.Today())
	})
	mux.HandleFunc("POST /v1/daily/replays", s.submitDaily)
	mux.HandleFunc("GET /v1/daily/leaderboard", func(w http.ResponseWriter, _ *http.Request) {
		today := s.replays.Today()
		writeJSON(w, http.StatusOK, map[string]any{"board": "daily-" + today.Date, "entries": s.boards.Top("daily-"+today.Date, 20)})
	})
	return mux
}

func (s *Server) submitDaily(w http.ResponseWriter, r *http.Request) {
	data, err := io.ReadAll(http.MaxBytesReader(w, r.Body, s.maxReplayBytes))
	if err != nil {
		var tooLarge *http.MaxBytesError
		if errors.As(err, &tooLarge) {
			writeError(w, apperr.TooLarge("replay_too_large", "replay exceeds the size limit"))
			return
		}
		writeError(w, apperr.BadRequest("bad_body", "cannot read the replay"))
		return
	}
	result, err := s.replays.SubmitDaily(r.Context(), r.URL.Query().Get("player"), data)
	if err != nil {
		writeError(w, err)
		return
	}
	writeJSON(w, http.StatusOK, result)
}

func writeJSON(w http.ResponseWriter, status int, v any) {
	w.Header().Set("Content-Type", "application/json; charset=utf-8")
	w.WriteHeader(status)
	if err := json.NewEncoder(w).Encode(v); err != nil {
		log.Printf("[transport] encode response: %v", err)
	}
}

// writeError маппит доменную ошибку в status + JSON {code, message}; прочие — в 500 без деталей.
func writeError(w http.ResponseWriter, err error) {
	var appErr *apperr.Error
	if errors.As(err, &appErr) {
		if appErr.Status >= http.StatusInternalServerError && appErr.Err != nil {
			log.Printf("[transport] %s: %v", appErr.Code, appErr.Err)
		}
		writeJSON(w, appErr.Status, appErr)
		return
	}
	log.Printf("[transport] unhandled error: %v", err)
	writeJSON(w, http.StatusInternalServerError, apperr.Internal("internal server error"))
}
