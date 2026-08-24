import { Link } from "react-router-dom";

function AppHeader() {
    return (
        <header className="app-header">
            <div className="app-header-inner">
                <Link to="/" className="brand-mark">
                    <span className="brand-name">Walking Patterns</span>
                    <span className="brand-subtitle">Interior Solutions</span>
                </Link>
                <Link to="/" className="header-logout">Logout</Link>
            </div>
        </header>
    );
}

export default AppHeader;
