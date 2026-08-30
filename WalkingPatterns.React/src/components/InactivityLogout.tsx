import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { toast } from "react-toastify";
import { useAuth } from "../auth/authContext";

const INACTIVITY_TIMEOUT_MS = 90 * 1000;
const ACTIVITY_THROTTLE_MS = 250;

export default function InactivityLogout() {
    const location = useLocation();
    const { isAuthenticated, logout } = useAuth();

    useEffect(() => {
        if (!isAuthenticated || location.pathname === "/login") return;

        let timer: number;
        let lastActivity = 0;
        const resetTimer = () => {
            const now = Date.now();
            if (now - lastActivity < ACTIVITY_THROTTLE_MS) return;
            lastActivity = now;
            window.clearTimeout(timer);
            timer = window.setTimeout(() => {
                toast.info("Session ended due to inactivity.");
                logout("inactive");
            }, INACTIVITY_TIMEOUT_MS);
        };
        const events = ["mousemove", "pointerdown", "keydown", "touchstart", "scroll"] as const;
        events.forEach(event => window.addEventListener(event, resetTimer, { passive: true }));
        resetTimer();
        return () => { window.clearTimeout(timer); events.forEach(event => window.removeEventListener(event, resetTimer)); };
    }, [isAuthenticated, location.pathname, logout]);

    return null;
}
