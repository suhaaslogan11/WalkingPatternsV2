import { Navigate, Outlet, useLocation } from "react-router-dom";
import { getToken } from "../services/authService";
export default function ProtectedRoute() { const location = useLocation(); return getToken() ? <Outlet /> : <Navigate to="/login" replace state={{ from: location }} />; }
