import api from "../api";
import { AUTH_TOKEN_KEY, clearToken, getToken, isTokenUsable, setToken } from "./authStorage";
const login = async (email: string, password: string) => { const response = await api.post("/Auth/login", { email, password }); const token = response.data.token; if (typeof token !== "string" || !isTokenUsable(token)) throw new Error("Login response did not include a valid token."); setToken(token); return token; };
const logout = clearToken;
export { AUTH_TOKEN_KEY, login, logout, getToken };
export default { login, logout, getToken };
