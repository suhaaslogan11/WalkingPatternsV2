import { Link } from "react-router-dom";
import { useAuth } from "../auth/authContext";

function AppHeader() {
    const { logout } = useAuth();
    return (
        <header className="app-header">
            <div className="app-header-inner">
                <Link to="/" className="brand-mark">
                    <span className="brand-name">Walking Patterns</span>
                    <span className="brand-subtitle">Interior Solutions</span>
                </Link>
                <button type="button" className="header-logout" onClick={() => logout("manual")}>Logout</button>
            </div>
        </header>
    );
}

export default AppHeader;
