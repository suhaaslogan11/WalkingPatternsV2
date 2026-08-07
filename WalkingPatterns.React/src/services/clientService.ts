import api from "../api";
import type { Client } from "../models/Client";

const getClients = async (): Promise<Client[]> => {
    const response = await api.get("/Clients");
    return response.data;
};

const getClient = async (id: number): Promise<Client> => {
    const response = await api.get(`/Clients/${id}`);
    return response.data;
};

const addClient = async (client: Client) => {
    const response = await api.post("/Clients", client);
    return response.data;
};

const updateClient = async (id: number, client: Client): Promise<Client> => {
    const response = await api.put(`/Clients/${id}`, client);
    return response.data;
};

const deleteClient = async (id: number) => {
    await api.delete(`/Clients/${id}`);
};

export default {
    getClients,
    getClient,
    addClient,
    updateClient,
    deleteClient
};