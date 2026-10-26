package replay

import (
	"os"
	"strings"
	"testing"
)

func TestParsesGoldenReplayHeader(t *testing.T) {
	data, err := os.ReadFile("../../../sim.tests/golden/afk-short-act.gfr")
	if err != nil {
		t.Fatal(err)
	}
	h, err := ParseHeader(data)
	if err != nil {
		t.Fatal(err)
	}
	if h.Seed != 13050482281110882231 || h.Balance == "" {
		t.Fatalf("unexpected header %+v", h)
	}
}

func TestRejectsIncompleteOrForeignReplays(t *testing.T) {
	cases := map[string]string{
		"no magic":    "balance x\nseed 1\ninputs AA\n",
		"bad format":  "glimmerfall-replay 9\nbalance x\nseed 1\ninputs AA\n",
		"bad seed":    "glimmerfall-replay 1\nbalance x\nseed -1\ninputs AA\n",
		"no inputs":   "glimmerfall-replay 1\nbalance x\nseed 1\n",
		"not replays": "hello",
	}
	for name, text := range cases {
		if _, err := ParseHeader([]byte(text)); err == nil {
			t.Errorf("%s: expected an error", name)
		}
	}
	if _, err := ParseHeader([]byte(strings.Join([]string{"# comment", "glimmerfall-replay 1", "balance x", "seed 7", "inputs AA"}, "\n"))); err != nil {
		t.Fatalf("valid minimal header rejected: %v", err)
	}
}
