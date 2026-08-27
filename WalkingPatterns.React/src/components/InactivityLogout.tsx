import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import authService from "../services/authService";

const INACTIVITY_TIMEOUT_MS = 90 * 1000;
const ACTIVITY_THROTTLE_MS = 250;

export default function InactivityLogout() {
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        if (!authService.getToken() || location.pathname === "/login") return;

        let timer: number;
        let lastActivity = 0;
        const resetTimer = () => {
            const now = Date.now();
            if (now - lastActivity < ACTIVITY_THROTTLE_MS) return;
            lastActivity = now;
            window.clearTimeout(timer);
            timer = window.setTimeout(() => {
                authService.logout();
                toast.info("Session ended due to inactivity.");
                navigate("/login", { replace: true });
            }, INACTIVITY_TIMEOUT_MS);
        };
        const events = ["mousemove", "pointerdown", "keydown", "touchstart", "scroll"] as const;
        events.forEach(event => window.addEventListener(event, resetTimer, { passive: true }));
        resetTimer();
        return () => { window.clearTimeout(timer); events.forEach(event => window.removeEventListener(event, resetTimer)); };
    }, [location.pathname, navigate]);

    return null;
}
