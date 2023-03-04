import { printf, toConsole } from "../Farss.Client/src/fable_modules/fable-library.3.7.18/String.js";

export const GetFileRoute = "/api/file/{id}";

export function GetFile(id) {
    toConsole(printf("/api/file/%A"))(id);
}

