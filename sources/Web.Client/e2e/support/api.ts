import type { APIRequestContext, APIResponse } from '@playwright/test';

const apiPath = '/api/v1';

export interface ISetupStatusResponse {
    isSetupRequired: boolean;
}

export interface ILoginCredentials {
    email: string;
    password: string;
}

export interface ISetupOwner {
    familyName: string;
    displayName: string;
    email: string;
    password: string;
}

const readJson = async <T>(response: APIResponse): Promise<T> => {
    return (await response.json()) as T;
};

const postJson = async (
    request: APIRequestContext,
    path: string,
    body?: object,
): Promise<APIResponse> => {
    return request.post(`${apiPath}${path}`, { data: body });
};

export const getSetupStatus = async (request: APIRequestContext): Promise<ISetupStatusResponse> => {
    const response = await request.get(`${apiPath}/setup/status`);

    if (!response.ok()) {
        throw new Error(`GET /setup/status failed with HTTP ${response.status()}`);
    }

    return readJson<ISetupStatusResponse>(response);
};

export const initializeInstance = async (
    request: APIRequestContext,
    owner: ISetupOwner,
): Promise<void> => {
    const response = await postJson(request, '/setup/initialize', {
        familyName: owner.familyName,
        displayName: owner.displayName,
        email: owner.email,
        password: owner.password,
        privacyAccepted: true,
    });

    if (!response.ok()) {
        throw new Error(`POST /setup/initialize failed with HTTP ${response.status()}`);
    }
};

interface IEntityWithId {
    id: number;
}

/** Deletes every learner and group of the signed-in family, so repeated runs stay below the family limits. */
export const deleteFamilyData = async (request: APIRequestContext): Promise<void> => {
    for (const area of ['groups', 'learners']) {
        const response = await request.get(`${apiPath}/${area}/list`);

        if (!response.ok()) {
            throw new Error(`GET /${area}/list failed with HTTP ${response.status()}`);
        }

        for (const { id } of await readJson<IEntityWithId[]>(response)) {
            const deleted = await postJson(request, `/${area}/delete`, { id });

            if (!deleted.ok()) {
                throw new Error(`POST /${area}/delete failed with HTTP ${deleted.status()}`);
            }
        }
    }
};

export const loginAsAdult = async (
    request: APIRequestContext,
    credentials: ILoginCredentials,
): Promise<void> => {
    const response = await postJson(request, '/authentication/login', credentials);

    if (!response.ok()) {
        throw new Error(`POST /authentication/login failed with HTTP ${response.status()}`);
    }
};
