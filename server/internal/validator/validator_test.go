package validator

import (
	"context"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// Интеграция с настоящим .NET-валидатором: GLIMMERFALL_SIM — путь к glimmerfall-sim (CI собирает его перед go test);
// без него тест пропускается. Записывает дейлик даты ботом и проверяет вердикты.
func TestRealValidatorAcceptsTheDailyAndRejectsAnotherDay(t *testing.T) {
	sim := os.Getenv("GLIMMERFALL_SIM")
	if sim == "" {
		t.Skip("GLIMMERFALL_SIM is not set — build sim.cli and point it at glimmerfall-sim")
	}
	replayPath := filepath.Join(t.TempDir(), "daily.gfr")
	record := exec.Command(sim, "record", "--daily", "2026-10-26", "--policy", "afk", "--out", replayPath)
	if out, err := record.CombinedOutput(); err != nil {
		t.Fatalf("record failed: %v\n%s", err, out)
	}
	data, err := os.ReadFile(replayPath)
	if err != nil {
		t.Fatal(err)
	}
	runner := Runner{Cmd: []string{sim, "verify", "--json", "-"}, Timeout: 2 * time.Minute}

	verdict, err := runner.VerifyDaily(context.Background(), "2026-10-26", data)
	if err != nil || !verdict.OK || verdict.Score <= 0 || verdict.Seed != "4973729189407420658" {
		t.Fatalf("daily must pass: %+v (%v)", verdict, err)
	}
	other, err := runner.VerifyDaily(context.Background(), "2026-10-27", data)
	if err != nil || other.OK || !strings.Contains(other.Message, "not the daily") {
		t.Fatalf("another day's replay must be rejected: %+v (%v)", other, err)
	}
	lines := strings.Split(string(data), "\n")
	for i, line := range lines {
		if strings.HasPrefix(line, "digest 600 ") {
			lines[i] = "digest 600 0000000000000000"
		}
	}
	broken, err := runner.VerifyDaily(context.Background(), "2026-10-26", []byte(strings.Join(lines, "\n")))
	if err != nil || broken.OK {
		t.Fatalf("tampered digest must be rejected: %+v (%v)", broken, err)
	}
}
