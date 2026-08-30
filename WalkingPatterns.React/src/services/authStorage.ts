export const AUTH_TOKEN_KEY = "walkingpatterns_token";
export const AUTH_STATE_CHANGED_EVENT = "walking-patterns-auth-changed";

function notifyAuthChanged() {
    window.dispatchEvent(new Event(AUTH_STATE_CHANGED_EVENT));
}

function decodeJwtPayload(token: string): { exp?: number } | null {
    try {
        const parts = token.split(".");
        if (parts.length !== 3 || !parts[1]) return null;

        const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
        const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
        return JSON.parse(atob(padded)) as { exp?: number };
    } catch {
        return null;
    }
}

export function isTokenUsable(token: string | null): boolean {
    if (!token) return false;

    const payload = decodeJwtPayload(token);
    if (!payload) return false;
    if (typeof payload.exp !== "number") return true;

    return payload.exp * 1000 > Date.now();
}

export function clearToken() {
    const hadToken = localStorage.getItem(AUTH_TOKEN_KEY) !== null;
    localStorage.removeItem(AUTH_TOKEN_KEY);
    if (hadToken) notifyAuthChanged();
}

export function getToken() {
    const token = localStorage.getItem(AUTH_TOKEN_KEY)?.trim() || null;
    if (!isTokenUsable(token)) {
        clearToken();
        return null;
    }

    return token;
}

export function setToken(token: string) {
    localStorage.setItem(AUTH_TOKEN_KEY, token);
    notifyAuthChanged();
}
