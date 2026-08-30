import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../auth/authContext";
import AppFooter from "./AppFooter";
import AppHeader from "./AppHeader";

export default function ProtectedRoute() {
    const location = useLocation();
    const { isAuthenticated } = useAuth();

    return isAuthenticated ? (
        <div className="app-shell">
            <AppHeader />
            <main className="app-main">
                <Outlet />
            </main>
            <AppFooter />
        </div>
    ) : (
        <Navigate to="/login" replace state={{ from: location }} />
    );
}
