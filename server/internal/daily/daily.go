// Package daily — день дейлика (PRD §10) по UTC. Правила дейлика (отряд, тир, мета, акты) живут в симе
// (sim/DailyRules.cs) и проверяются валидатором; сервер знает только дату и сид — для дешёвого предотсева.
package daily

import "time"

// Daily — сегодняшний дейлик с точки зрения сервера.
type Daily struct {
	Date     string `json:"date"`
	SeedText string `json:"seedText"`
	Seed     uint64 `json:"seed,string"`
}

// For возвращает дейлик на дату (день по UTC).
func For(now time.Time) Daily {
	date := now.UTC().Format("2006-01-02")
	seedText := "daily-" + date
	return Daily{Date: date, SeedText: seedText, Seed: SeedFromString(seedText)}
}

// SeedFromString — FNV-1a 64 по UTF-16 единицам строки (младший байт, затем старший), как C# SeedHash.FromString.
func SeedFromString(text string) uint64 {
	const offset = 14695981039346656037
	const prime = 1099511628211
	hash := uint64(offset)
	for _, unit := range utf16Units(text) {
		hash ^= uint64(unit & 0xFF)
		hash *= prime
		hash ^= uint64(unit >> 8)
		hash *= prime
	}
	return hash
}

func utf16Units(text string) []uint16 {
	units := make([]uint16, 0, len(text))
	for _, r := range text {
		if r >= 0x10000 {
			r -= 0x10000
			units = append(units, uint16(0xD800+(r>>10)), uint16(0xDC00+(r&0x3FF)))
			continue
		}
		units = append(units, uint16(r))
	}
	return units
}
