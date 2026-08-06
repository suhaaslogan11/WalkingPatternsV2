import { useState } from "react";
import { useNavigate } from "react-router-dom";
import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientForm() {

    const navigate = useNavigate();

    const [client, setClient] = useState<Client>({
        clientId: 0,
        clientName: "",
        phone: "",
        email: ""
    });

    const handleChange = (
        e: React.ChangeEvent<HTMLInputElement>
    ) => {

        setClient({
            ...client,
            [e.target.name]: e.target.value
        });

    };

    const handleSubmit = async (
        e: React.FormEvent
    ) => {

        e.preventDefault();

        try {

            await clientService.addClient(client);

            alert("Client Added Successfully");

            navigate("/");

        }
        catch (error) {

            console.error(error);

            alert("Unable to save client");

        }

    };

    return (

        <div className="container mt-5">

            <h2>Add Client</h2>

            <form onSubmit={handleSubmit}>

                <div className="mb-3">

                    <label className="form-label">
                        Client Name
                    </label>

                    <input
                        type="text"
                        name="clientName"
                        className="form-control"
                        value={client.clientName}
                        onChange={handleChange}
                    />

                </div>

                <div className="mb-3">

                    <label className="form-label">
                        Phone
                    </label>

                    <input
                        type="text"
                        name="phone"
                        className="form-control"
                        value={client.phone}
                        onChange={handleChange}
                    />

                </div>

                <div className="mb-3">

                    <label className="form-label">
                        Email
                    </label>

                    <input
                        type="email"
                        name="email"
                        className="form-control"
                        value={client.email}
                        onChange={handleChange}
                    />

                </div>

                <button
                    className="btn btn-success"
                    type="submit">

                    Save

                </button>

            </form>

        </div>

    );

}

export default ClientForm;