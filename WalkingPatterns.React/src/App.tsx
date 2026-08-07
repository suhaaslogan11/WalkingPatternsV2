import { Routes, Route } from "react-router-dom";
import ClientList from "./pages/clients/ClientList";
import ClientForm from "./pages/clients/ClientForm";

function App() {

    return (

        <Routes>

            <Route
                path="/"
                element={<ClientList />}
            />

            <Route
                path="/clients/add"
                element={<ClientForm />}
            />

            <Route
                path="/clients/edit/:id"
                element={<ClientForm />}
            />

        </Routes>

    );

}

export default App;