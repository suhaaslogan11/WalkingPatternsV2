import axios from "axios";
const TOKEN_KEY = "walkingpatterns_token";

const api = axios.create({
    baseURL: "https://localhost:7232/api" // Change if your API uses a different port
});

api.interceptors.request.use(config => { const token = localStorage.getItem(TOKEN_KEY); if (token) config.headers.Authorization = `Bearer ${token}`; return config; });
api.interceptors.response.use(response => response, error => { if (error.response?.status === 401 && !String(error.config?.url || "").toLowerCase().includes("/auth/login")) { localStorage.removeItem(TOKEN_KEY); if (window.location.pathname !== "/login") window.location.href = "/login"; } return Promise.reject(error); });

export default api;
