package transport

import (
	"context"
	"encoding/json"
	"fmt"
	"net/http"
	"net/http/httptest"
	"strings"
	"testing"
	"time"

	"github.com/ArtemChikolaev/Glimmerfall/server/internal/daily"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/service"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/store"
	"github.com/ArtemChikolaev/Glimmerfall/server/internal/validator"
)

var testNow = time.Date(2026, 10, 26, 12, 0, 0, 0, time.UTC)

type fakeVerifier struct {
	verdict validator.Verdict
	date    string
}

func (f *fakeVerifier) VerifyDaily(_ context.Context, date string, _ []byte) (validator.Verdict, error) {
	f.date = date
	return f.verdict, nil
}

func newTestServer(v *fakeVerifier, maxBytes int64) (*httptest.Server, *store.Leaderboards) {
	boards := store.NewLeaderboards()
	replays := &service.Replays{Verifier: v, Boards: boards, Now: func() time.Time { return testNow }}
	return httptest.NewServer(NewServer(replays, boards, maxBytes).Routes()), boards
}

func replayFor(seed uint64) string {
	return fmt.Sprintf("glimmerfall-replay 1\nbalance 0.1.0-spike\nseed %d\nconfig scenario=0\ninputs AA\n", seed)
}

func post(t *testing.T, url, body string) (*http.Response, map[string]any) {
	t.Helper()
	resp, err := http.Post(url, "text/plain", strings.NewReader(body))
	if err != nil {
		t.Fatal(err)
	}
	defer resp.Body.Close()
	var payload map[string]any
	_ = json.NewDecoder(resp.Body).Decode(&payload)
	return resp, payload
}

func TestAcceptsTodaysDailyAndRanksByValidatorScore(t *testing.T) {
	today := daily.For(testNow)
	v := &fakeVerifier{verdict: validator.Verdict{OK: true, Seed: fmt.Sprint(today.Seed), Score: 1234, Ticks: 9000, Outcome: "Defeat"}}
	srv, boards := newTestServer(v, 1<<20)
	defer srv.Close()

	resp, payload := post(t, srv.URL+"/v1/daily/replays?player=owner", replayFor(today.Seed))
	if resp.StatusCode != http.StatusOK {
		t.Fatalf("status %d: %v", resp.StatusCode, payload)
	}
	if payload["rank"].(float64) != 1 || v.date != "2026-10-26" {
		t.Fatalf("unexpected result %v (validator date %q)", payload, v.date)
	}
	if top := boards.Top("daily-2026-10-26", 5); len(top) != 1 || top[0].Score != 1234 {
		t.Fatalf("score must come from the validator verdict: %+v", top)
	}
}

func TestRejections(t *testing.T) {
	today := daily.For(testNow)
	cases := []struct {
		name    string
		verdict validator.Verdict
		url     string
		body    string
		status  int
		code    string
	}{
		{"wrong day", validator.Verdict{OK: true}, "/v1/daily/replays?player=p", replayFor(today.Seed + 1), http.StatusUnprocessableEntity, "not_today_daily"},
		{"validator rejects", validator.Verdict{OK: false, Message: "digest mismatch at step 600"}, "/v1/daily/replays?player=p", replayFor(today.Seed), http.StatusUnprocessableEntity, "replay_rejected"},
		{"no player", validator.Verdict{OK: true}, "/v1/daily/replays", replayFor(today.Seed), http.StatusBadRequest, "player_required"},
		{"garbage", validator.Verdict{OK: true}, "/v1/daily/replays?player=p", "hello", http.StatusBadRequest, "bad_replay"},
		{"too large", validator.Verdict{OK: true}, "/v1/daily/replays?player=p", strings.Repeat("x", 2048), http.StatusRequestEntityTooLarge, "replay_too_large"},
	}
	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			srv, boards := newTestServer(&fakeVerifier{verdict: c.verdict}, 1024)
			defer srv.Close()
			resp, payload := post(t, srv.URL+c.url, c.body)
			if resp.StatusCode != c.status || payload["code"] != c.code {
				t.Fatalf("status %d payload %v, want %d %s", resp.StatusCode, payload, c.status, c.code)
			}
			if len(boards.Top("daily-2026-10-26", 5)) != 0 {
				t.Fatal("rejected replay must not reach the leaderboard")
			}
		})
	}
}

func TestDailyEndpoint(t *testing.T) {
	srv, _ := newTestServer(&fakeVerifier{}, 1024)
	defer srv.Close()
	resp, err := http.Get(srv.URL + "/v1/daily")
	if err != nil {
		t.Fatal(err)
	}
	defer resp.Body.Close()
	var d daily.Daily
	if err := json.NewDecoder(resp.Body).Decode(&d); err != nil || d.SeedText != "daily-2026-10-26" {
		t.Fatalf("unexpected daily %+v (%v)", d, err)
	}
}
