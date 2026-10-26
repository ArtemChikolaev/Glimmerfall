// Package store — хранилище каркаса: таблицы рекордов в памяти одного инстанса.
// Postgres (по образцу aegis-draft) и trusted-запись в лидерборды Steam — следующий шаг (BACKLOG G1.6/G1.7).
package store

import (
	"sort"
	"sync"
	"time"
)

// Entry — запись таблицы. Очки и дайджест — из вердикта валидатора, не от клиента.
type Entry struct {
	Player   string    `json:"player"`
	Score    int       `json:"score"`
	Ticks    int       `json:"ticks"`
	Outcome  string    `json:"outcome"`
	Digest   string    `json:"digest"`
	Accepted time.Time `json:"accepted"`
}

// Leaderboards — таблицы по имени («daily-2026-10-26»). Лучшая запись игрока заменяет его прежнюю.
type Leaderboards struct {
	mu     sync.Mutex
	boards map[string][]Entry
}

func NewLeaderboards() *Leaderboards {
	return &Leaderboards{boards: map[string][]Entry{}}
}

// Submit сохраняет запись, если она лучше прежней записи игрока, и возвращает место игрока (с 1).
func (l *Leaderboards) Submit(board string, entry Entry) int {
	l.mu.Lock()
	defer l.mu.Unlock()
	entries := l.boards[board]
	replaced := false
	for i, existing := range entries {
		if existing.Player != entry.Player {
			continue
		}
		if better(entry, existing) {
			entries[i] = entry
		}
		replaced = true
		break
	}
	if !replaced {
		entries = append(entries, entry)
	}
	sort.SliceStable(entries, func(i, j int) bool { return better(entries[i], entries[j]) })
	l.boards[board] = entries
	for i, e := range entries {
		if e.Player == entry.Player {
			return i + 1
		}
	}
	return 0
}

// Top возвращает до n лучших записей.
func (l *Leaderboards) Top(board string, n int) []Entry {
	l.mu.Lock()
	defer l.mu.Unlock()
	entries := l.boards[board]
	if len(entries) > n {
		entries = entries[:n]
	}
	return append([]Entry(nil), entries...)
}

// better: больше очков, при равенстве — быстрее (меньше тиков), затем раньше принятая.
func better(a, b Entry) bool {
	if a.Score != b.Score {
		return a.Score > b.Score
	}
	if a.Ticks != b.Ticks {
		return a.Ticks < b.Ticks
	}
	return a.Accepted.Before(b.Accepted)
}
