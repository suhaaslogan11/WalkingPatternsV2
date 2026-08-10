import api from "../api";
import type {
    AddProjectRequest,
    Project,
    ProjectDetailPage,
    ProjectOrders
} from "../models/Project";

const getProjects = async (clientId: number): Promise<Project[]> => {
    const response = await api.get(`/clients/${clientId}/projects`);
    return response.data;
};

const getProject = async (id: number): Promise<Project> => {
    const response = await api.get(`/projects/${id}`);
    return response.data;
};

const getProjectDetails = async (projectId: number): Promise<ProjectDetailPage> => {
    const response = await api.get(`/projects/${projectId}/details`);
    return response.data;
};

const getProjectDetailOrders = async (projectDetailId: number): Promise<ProjectOrders> => {
    const response = await api.get(`/project-details/${projectDetailId}/orders`);
    return response.data;
};

const addProject = async (
    clientId: number,
    project: AddProjectRequest
): Promise<Project> => {
    const response = await api.post(`/clients/${clientId}/projects`, project);
    return response.data;
};

const deleteProject = async (id: number) => {
    await api.delete(`/projects/${id}`);
};

const updateProject = async (
    id: number,
    project: AddProjectRequest
): Promise<Project> => {
    const response = await api.put(`/projects/${id}`, project);
    return response.data;
};

export default {
    getProjects,
    getProject,
    getProjectDetails,
    getProjectDetailOrders,
    addProject,
    deleteProject,
    updateProject
};
