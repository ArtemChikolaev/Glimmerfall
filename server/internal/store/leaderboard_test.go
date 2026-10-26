package store

import (
	"testing"
	"time"
)

func TestBestEntryPerPlayerAndRanking(t *testing.T) {
	l := NewLeaderboards()
	t0 := time.Date(2026, 10, 26, 12, 0, 0, 0, time.UTC)
	if rank := l.Submit("d", Entry{Player: "a", Score: 100, Ticks: 500, Accepted: t0}); rank != 1 {
		t.Fatalf("first entry rank %d", rank)
	}
	if rank := l.Submit("d", Entry{Player: "b", Score: 200, Ticks: 900, Accepted: t0}); rank != 1 {
		t.Fatalf("higher score must lead, rank %d", rank)
	}
	if rank := l.Submit("d", Entry{Player: "a", Score: 50, Ticks: 100, Accepted: t0}); rank != 2 {
		t.Fatalf("worse retry must keep the old best, rank %d", rank)
	}
	if rank := l.Submit("d", Entry{Player: "c", Score: 200, Ticks: 800, Accepted: t0}); rank != 1 {
		t.Fatalf("same score, fewer ticks must win the tie, rank %d", rank)
	}
	top := l.Top("d", 10)
	if len(top) != 3 || top[0].Player != "c" || top[1].Player != "b" || top[2].Player != "a" || top[2].Score != 100 {
		t.Fatalf("unexpected order %+v", top)
	}
}
