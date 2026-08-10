import { useCallback, useEffect } from "react";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";

import clientService from "../../services/clientService";
import type { Client } from "../../models/Client";

function ClientForm() {

    const navigate = useNavigate();
    const { id } = useParams();

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors }
    } = useForm<Client>();

    const loadClient = useCallback(async () => {

        try {

            const data = await clientService.getClient(Number(id));

            setValue("clientId", data.clientId);
            setValue("clientName", data.clientName);
            setValue("phone", data.phone);
            setValue("email", data.email);

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to load client");

        }

    }, [id, setValue]);

    useEffect(() => {

        if (id) {
            loadClient();
        }

    }, [id, loadClient]);

    const onSubmit = async (data: Client) => {

        try {

            if (id) {

                await clientService.updateClient(Number(id), data);

                toast.success("Client Updated Successfully");

            }
            else {

                await clientService.addClient(data);

                toast.success("Client Added Successfully");

            }

            navigate("/");

        }
        catch (error) {

            console.error(error);

            toast.error("Unable to save client");

        }

    };

    return (

        <div className="container mt-5">

            <h2 className="mb-4">

                {id ? "Edit Client" : "Add Client"}

            </h2>

            <form onSubmit={handleSubmit(onSubmit)}>

                <div className="mb-3">

                    <label className="form-label">
                        Client Name
                    </label>

                    <input
                        type="text"
                        className="form-control"
                        {...register("clientName", {
                            required: "Client Name is required"
                        })}
                    />

                    {errors.clientName && (

                        <div className="text-danger">
                            {errors.clientName.message}
                        </div>

                    )}

                </div>

                <div className="mb-3">

                    <label className="form-label">
                        Phone
                    </label>

                    <input
                        type="text"
                        className="form-control"
                        {...register("phone", {
                            required: "Phone is required",
                            pattern: {
                                value: /^[0-9]{10}$/,
                                message: "Phone must be 10 digits"
                            }
                        })}
                    />

                    {errors.phone && (

                        <div className="text-danger">
                            {errors.phone.message}
                        </div>

                    )}

                </div>

                <div className="mb-3">

                    <label className="form-label">
                        Email
                    </label>

                    <input
                        type="email"
                        className="form-control"
                        {...register("email", {
                            required: "Email is required",
                            pattern: {
                                value: /^\S+@\S+\.\S+$/,
                                message: "Invalid Email"
                            }
                        })}
                    />

                    {errors.email && (

                        <div className="text-danger">
                            {errors.email.message}
                        </div>

                    )}

                </div>

                <button
                    type="submit"
                    className="btn btn-success"
                >
                    {id ? "Update" : "Save"}
                </button>

                <button
                    type="button"
                    className="btn btn-secondary ms-2"
                    onClick={() => navigate("/")}
                >
                    Cancel
                </button>

            </form>

        </div>

    );

}

export default ClientForm;
