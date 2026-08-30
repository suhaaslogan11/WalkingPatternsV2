import axios from "axios";
import { clearToken, getToken } from "./services/authStorage";

const api = axios.create({
    baseURL: "https://localhost:7232/api" // Change if your API uses a different port
});

api.interceptors.request.use(config => { const token = getToken(); if (token) config.headers.Authorization = `Bearer ${token}`; return config; });
api.interceptors.response.use(response => response, error => { if (error.response?.status === 401 && !String(error.config?.url || "").toLowerCase().includes("/auth/login")) clearToken(); return Promise.reject(error); });

export default api;
