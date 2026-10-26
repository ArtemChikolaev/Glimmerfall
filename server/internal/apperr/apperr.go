// Package apperr — единый доменный тип ошибки (образец aegis-draft): хендлеры возвращают *Error,
// transport маппит его в HTTP status + JSON {code, message}.
package apperr

import "net/http"

// Error — доменная ошибка: машинный code, человекочитаемое message и HTTP status.
type Error struct {
	Status  int    `json:"-"`
	Code    string `json:"code"`
	Message string `json:"message"`
	// Err — внутренняя причина; клиенту не уходит, для 5xx пишется в лог.
	Err error `json:"-"`
}

func (e *Error) Error() string { return e.Message }

// Unwrap — чтобы errors.Is/As видели причину.
func (e *Error) Unwrap() error { return e.Err }

// Wrap прикладывает причину и возвращает ту же ошибку.
func (e *Error) Wrap(err error) *Error {
	e.Err = err
	return e
}

// New конструирует доменную ошибку.
func New(status int, code, message string) *Error {
	return &Error{Status: status, Code: code, Message: message}
}

func BadRequest(code, message string) *Error { return New(http.StatusBadRequest, code, message) }
func TooLarge(code, message string) *Error {
	return New(http.StatusRequestEntityTooLarge, code, message)
}
func Unprocessable(code, message string) *Error {
	return New(http.StatusUnprocessableEntity, code, message)
}
func Internal(message string) *Error { return New(http.StatusInternalServerError, "internal", message) }
