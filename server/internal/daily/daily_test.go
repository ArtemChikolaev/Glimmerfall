package daily

import (
	"testing"
	"time"
)

// Вектор снят с C#: glimmerfall-sim record --daily 2026-10-26 печатает seed 4973729189407420658.
func TestSeedMatchesCSharpSeedHash(t *testing.T) {
	if got := SeedFromString("daily-2026-10-26"); got != 4973729189407420658 {
		t.Fatalf("SeedFromString = %d, want C# value 4973729189407420658", got)
	}
	if got := SeedFromString(""); got != 14695981039346656037 {
		t.Fatalf("empty string must hash to the FNV offset, got %d", got)
	}
}

func TestDayIsTakenInUTC(t *testing.T) {
	moscow := time.FixedZone("MSK", 3*60*60)
	d := For(time.Date(2026, 10, 27, 1, 30, 0, 0, moscow))
	if d.Date != "2026-10-26" || d.SeedText != "daily-2026-10-26" || d.Seed != 4973729189407420658 {
		t.Fatalf("unexpected daily %+v", d)
	}
}
