import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useLocation, useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import type { AddProjectRequest, Project } from "../../models/Project";
import projectService from "../../services/projectService";

function ProjectForm() {

    const navigate = useNavigate();
    const location = useLocation();
    const { id } = useParams();
    const [project, setProject] = useState<Project | undefined>(
        location.state?.project as Project | undefined
    );

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors }
    } = useForm<AddProjectRequest>();

    useEffect(() => {

        if (project) {
            const [day, month, year] = project.projectDate.split("-");

            setValue("projectName", project.projectName);
            setValue("projectDate", `${year}-${month}-${day}`);
            setValue("versionNumber", project.versionNumber);
            return;
        }

        if (!id)
            return;

        void projectService.getProject(Number(id))
            .then(setProject)
            .catch((error) => {
                console.error(error);
                toast.error("Unable to load project");
            });

    }, [id, project, setValue]);

    const onSubmit = async (data: AddProjectRequest) => {

        if (!id || !project)
            return;

        const formattedProjectDate = data.projectDate
            .split("-")
            .reverse()
            .join("-");

        try {

            await projectService.updateProject(Number(id), {
                ...data,
                projectDate: formattedProjectDate
            });

            toast.success("Project updated successfully");
            navigate(`/clients/${project.clientId}/projects`);

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to update project");

        }

    };

    if (!project) {
        return (
            <div className="container mt-5">
                <div className="alert alert-danger">Project details are unavailable.</div>
                <Link to="/" className="btn btn-secondary">Back to Clients</Link>
            </div>
        );
    }

    return (

        <div className="container py-3">

            <h2 className="page-title mb-4">Edit Project</h2>

            <form className="border rounded p-3 bg-white mx-auto" style={{ maxWidth: 680 }} onSubmit={handleSubmit(onSubmit)} noValidate>

                <div className="mb-3">
                    <label className="form-label">Project Name</label>
                    <input
                        type="text"
                        className="form-control"
                        {...register("projectName", {
                            required: "Project name is required"
                        })}
                    />
                    {errors.projectName && (
                        <div className="text-danger">
                            {errors.projectName.message}
                        </div>
                    )}
                </div>

                <div className="mb-3">
                    <label className="form-label">Project Date</label>
                    <input
                        type="date"
                        className="form-control"
                        {...register("projectDate", {
                            required: "Project date is required"
                        })}
                    />
                    {errors.projectDate && (
                        <div className="text-danger">
                            {errors.projectDate.message}
                        </div>
                    )}
                </div>

                <div className="form-actions"><button type="submit" className="btn btn-primary">
                    Update
                </button>

                <button
                    type="button"
                    className="btn btn-outline-secondary"
                    onClick={() => navigate(`/clients/${project.clientId}/projects`)}
                >
                    Cancel
                </button></div>

            </form>

        </div>

    );
}

export default ProjectForm;
