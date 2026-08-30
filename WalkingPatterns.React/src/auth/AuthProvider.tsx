import { useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import authService from "../services/authService";
import { AUTH_STATE_CHANGED_EVENT } from "../services/authStorage";
import { AuthContext } from "./authContext";

export default function AuthProvider({ children }: { children: ReactNode }) {
    const navigate = useNavigate();
    const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(authService.getToken()));

    useEffect(() => {
        const syncAuthState = () => setIsAuthenticated(Boolean(authService.getToken()));
        window.addEventListener(AUTH_STATE_CHANGED_EVENT, syncAuthState);
        window.addEventListener("storage", syncAuthState);
        return () => {
            window.removeEventListener(AUTH_STATE_CHANGED_EVENT, syncAuthState);
            window.removeEventListener("storage", syncAuthState);
        };
    }, []);

    const login = useCallback(async (email: string, password: string) => {
        await authService.login(email, password);
        setIsAuthenticated(true);
    }, []);

    const logout = useCallback(() => {
        authService.logout();
        setIsAuthenticated(false);
        navigate("/login", { replace: true });
    }, [navigate]);

    const value = useMemo(() => ({ isAuthenticated, login, logout }), [isAuthenticated, login, logout]);

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
