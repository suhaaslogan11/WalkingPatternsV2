import api from "../api";
import type { AddProjectRequest, Project } from "../models/Project";

const getProjects = async (clientId: number): Promise<Project[]> => {
    const response = await api.get(`/clients/${clientId}/projects`);
    return response.data;
};

const getProject = async (id: number): Promise<Project> => {
    const response = await api.get(`/projects/${id}`);
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
    addProject,
    deleteProject,
    updateProject
};
