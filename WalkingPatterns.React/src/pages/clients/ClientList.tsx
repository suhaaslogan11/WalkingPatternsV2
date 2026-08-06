import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientList() {

    const [clients, setClients] = useState<Client[]>([]);

    useEffect(() => {
        loadClients();
    }, []);

    const loadClients = async () => {

        try {

            const data = await clientService.getClients();

            setClients(data);

        }
        catch (error) {

            console.error(error);

        }

    };

    return (

        <div className="container mt-5">

            <div className="d-flex justify-content-between align-items-center mb-4">

                <h2>Clients</h2>

                <Link
                    to="/clients/add"
                    className="btn btn-primary"
                >
                    Add Client
                </Link>

            </div>

            <table className="table table-striped table-hover table-bordered">

                <thead>

                    <tr>
                        <th>Id</th>
                        <th>Name</th>
                        <th>Phone</th>
                        <th>Email</th>
                    </tr>

                </thead>

                <tbody>

                    {clients.map((client) => (

                        <tr key={client.clientId}>

                            <td>{client.clientId}</td>
                            <td>{client.clientName}</td>
                            <td>{client.phone}</td>
                            <td>{client.email}</td>

                        </tr>

                    ))}

                </tbody>

            </table>

        </div>

    );

}

export default ClientList;