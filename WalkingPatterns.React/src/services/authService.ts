import api from "../api";
const TOKEN_KEY = "walkingpatterns_token";
const login = async (email: string, password: string) => { const response = await api.post("/Auth/login", { email, password }); const token = response.data.token; if (typeof token !== "string" || !token) throw new Error("Login response did not include a token."); localStorage.setItem(TOKEN_KEY, token); return token; };
const logout = () => localStorage.removeItem(TOKEN_KEY);
const getToken = () => localStorage.getItem(TOKEN_KEY);
export { TOKEN_KEY, login, logout, getToken };
export default { login, logout, getToken };
