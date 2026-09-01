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

                toast.success("Client updated successfully");

            }
            else {

                await clientService.addClient(data);

                toast.success("Client added successfully");

            }

            navigate("/");

        }
        catch (error) {

            console.error(error);

            toast.error("Unable to save client");

        }

    };

    return (

        <div className="container py-3">

            <h2 className="page-title mb-4">

                {id ? "Edit Client" : "Add Client"}

            </h2>

            <form className="border rounded p-3 bg-white mx-auto" style={{ maxWidth: 680 }} onSubmit={handleSubmit(onSubmit)} noValidate>

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
                                value: /^[6-9]\d{9}$/,
                                message: "Enter a valid 10-digit Indian mobile number"
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
                                message: "Enter a valid email address"
                            }
                        })}
                    />

                    {errors.email && (

                        <div className="text-danger">
                            {errors.email.message}
                        </div>

                    )}

                </div>

                <div className="client-form-actions"><button
                    type="submit"
                    className="btn btn-primary w-100"
                >
                    {id ? "Update" : "Save"}
                </button>

                <button
                    type="button"
                    className="btn btn-outline-secondary w-100"
                    onClick={() => navigate("/")}
                >
                    Cancel
                </button></div>

            </form>

        </div>

    );

}

export default ClientForm;
