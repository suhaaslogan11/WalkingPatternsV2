import axios from "axios";

const api = axios.create({
    baseURL: "https://localhost:7232/api" // Change if your API uses a different port
});

export default api;