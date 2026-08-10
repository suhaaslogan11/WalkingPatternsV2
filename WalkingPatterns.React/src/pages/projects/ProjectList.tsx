import { useCallback, useEffect, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useParams } from "react-router-dom";
import { toast } from "react-toastify";
import type { AddProjectRequest, Project } from "../../models/Project";
import projectService from "../../services/projectService";

function ProjectList() {

    const { clientId } = useParams();
    const parsedClientId = Number(clientId);
    const isValidClientId = useMemo(
        () => Number.isInteger(parsedClientId) && parsedClientId > 0,
        [parsedClientId]
    );
    const [projects, setProjects] = useState<Project[]>([]);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors }
    } = useForm<AddProjectRequest>();

    const loadProjects = useCallback(async () => {

        if (!isValidClientId)
            return;

        try {

            const data = await projectService.getProjects(parsedClientId);

            setProjects(data);

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to load projects");

        }

    }, [isValidClientId, parsedClientId]);

    useEffect(() => {

        if (!isValidClientId)
            return;

        void projectService.getProjects(parsedClientId)
            .then(setProjects)
            .catch((error) => {
                console.error(error);
                toast.error("Unable to load projects");
            });

    }, [isValidClientId, parsedClientId]);

    const onSubmit = async (data: AddProjectRequest) => {

        const formattedProjectDate = data.projectDate
            .split("-")
            .reverse()
            .join("-");

        try {

            await projectService.addProject(parsedClientId, {
                ...data,
                projectDate: formattedProjectDate
            });

            reset();
            await loadProjects();
            toast.success("Project added successfully");

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to add project");

        }

    };

    const handleDelete = async (id: number) => {

        if (!window.confirm("Are you sure you want to delete this project?"))
            return;

        try {

            await projectService.deleteProject(id);
            await loadProjects();
            toast.success("Project deleted successfully");

        }
        catch (error) {

            console.error(error);
            toast.error("Unable to delete project");

        }

    };

    if (!isValidClientId) {
        return (
            <div className="container mt-5">
                <div className="alert alert-danger">Invalid client.</div>
                <Link to="/" className="btn btn-secondary">Back to Clients</Link>
            </div>
        );
    }

    return (

        <div className="container mt-5">

            <div className="d-flex justify-content-between align-items-center mb-4">

                <h2 className="mb-0">Projects</h2>

                <Link to="/" className="btn btn-secondary">
                    Back to Clients
                </Link>

            </div>

            <div className="card shadow mb-4">

                <div className="card-header">
                    <h4 className="mb-0">Add Project</h4>
                </div>

                <div className="card-body">

                    <form onSubmit={handleSubmit(onSubmit)} noValidate>

                        <div className="row g-3 align-items-end">

                            <div className="col-md-5">
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

                            <div className="col-md-4">
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

                            <div className="col-md-3">
                                <button type="submit" className="btn btn-primary w-100">
                                    Add Project
                                </button>
                            </div>

                        </div>

                    </form>

                </div>

            </div>

            <div className="card shadow">

                <div className="card-header">
                    <h4 className="mb-0">Client Projects</h4>
                </div>

                <div className="card-body">

                    <table className="table table-striped table-hover table-bordered mb-0">

                        <thead className="table-dark">
                            <tr>
                                <th>Project Name</th>
                                <th>Project Date</th>
                                <th>Version</th>
                                <th style={{ width: "180px" }}>Actions</th>
                            </tr>
                        </thead>

                        <tbody>
                            {projects.length === 0 ? (
                                <tr>
                                    <td colSpan={4} className="text-center">
                                        No Projects Found
                                    </td>
                                </tr>
                            ) : (
                                projects.map((project) => (
                                    <tr key={project.id}>
                                        <td>{project.projectName}</td>
                                        <td>{project.projectDate}</td>
                                        <td>{project.versionNumber}</td>
                                        <td>
                                            <Link
                                                to={`/projects/edit/${project.id}`}
                                                state={{ project }}
                                                className="btn btn-warning btn-sm me-2"
                                            >
                                                Edit
                                            </Link>
                                            <button
                                                type="button"
                                                className="btn btn-danger btn-sm"
                                                onClick={() => handleDelete(project.id)}
                                            >
                                                Delete
                                            </button>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>

                    </table>

                </div>

            </div>

        </div>

    );
}

export default ProjectList;
