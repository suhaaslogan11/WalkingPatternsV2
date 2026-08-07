import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientForm() {

    const navigate = useNavigate();
    const { id } = useParams();

    const [client, setClient] = useState<Client>({
        clientId: 0,
        clientName: "",
        phone: "",
        email: ""
    });

    useEffect(() => {

        if (id) {
            loadClient();
        }

    }, [id]);

    const loadClient = async () => {

        try {

            const data = await clientService.getClient(Number(id));

            setClient(data);

        }
        catch (error) {

            console.error(error);

            alert("Unable to load client");

        }

    };

    

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

            if (id) {

                await clientService.updateClient(client);

                alert("Client Updated Successfully");

            }
            else {

                await clientService.addClient(client);

                alert("Client Added Successfully");

            }

            navigate("/");

        }
        catch (error) {

            console.error(error);

            alert("Unable to save client");

        }

    };

    return (

        <div className="container mt-5">

            <div className="card shadow">

                <div className="card-header">

                    <h3>
                        {id ? "Edit Client" : "Add Client"}
                    </h3>

                </div>

                <div className="card-body">

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
                                required
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
                            type="submit"
                            className="btn btn-success me-2">

                            {id ? "Update" : "Save"}

                        </button>

                        <button
                            type="button"
                            className="btn btn-secondary"
                            onClick={() => navigate("/")}>
                            Cancel
                        </button>

                    </form>

                </div>

            </div>

        </div>

    );

}

export default ClientForm;