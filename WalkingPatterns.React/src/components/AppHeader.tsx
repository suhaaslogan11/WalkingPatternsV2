import { Link } from "react-router-dom";
import { useLocation, useNavigate } from "react-router-dom";
import { getToken, logout } from "../services/authService";

function AppHeader() {
    const navigate = useNavigate();
    useLocation();
    if (!getToken()) return null;
    return (
        <header className="app-header">
            <div className="app-header-inner">
                <Link to="/" className="brand-mark">
                    <span className="brand-name">Walking Patterns</span>
                    <span className="brand-subtitle">Interior Solutions</span>
                </Link>
                <button type="button" className="header-logout" onClick={() => { logout(); navigate("/login", { replace: true }); }}>Logout</button>
            </div>
        </header>
    );
}

export default AppHeader;
