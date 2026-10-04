import type { APIRequestContext } from '@playwright/test';

interface IMailpitMessageListResponse {
    messages?: IMailpitMessage[];
}

interface IMailpitMessage {
    ID: string;
    To?: Array<{ Address?: string }>;
}

interface IMailpitMessageTextResponse {
    Text?: string;
    HTML?: string;
}

const mailpitBaseUrl = 'http://localhost:8025/api/v1';
const linkRegex = /https?:\/\/[^\s"'<>()]+/g;

const extractLinks = (content: string): string[] => {
    return content.match(linkRegex) ?? [];
};

export const findLatestMailIdFor = async (
    request: APIRequestContext,
    email: string,
): Promise<string | null> => {
    const response = await request.get(`${mailpitBaseUrl}/messages`);

    if (!response.ok()) {
        throw new Error(`Mailpit /messages failed with HTTP ${response.status()}`);
    }

    const data = (await response.json()) as IMailpitMessageListResponse;
    const messages = data.messages ?? [];

    for (const message of messages) {
        const recipients = message.To ?? [];
        const isMatch = recipients.some(
            (recipient) => recipient.Address?.toLowerCase() === email.toLowerCase(),
        );

        if (isMatch) {
            return message.ID;
        }
    }

    return null;
};

export const extractLinksFromMail = async (
    request: APIRequestContext,
    messageId: string,
): Promise<string[]> => {
    const response = await request.get(`${mailpitBaseUrl}/message/${messageId}`);

    if (!response.ok()) {
        throw new Error(`Mailpit /message/{id} failed with HTTP ${response.status()}`);
    }

    const body = (await response.json()) as IMailpitMessageTextResponse;
    const text = body.Text ?? body.HTML ?? '';
    return extractLinks(text);
};

export const findLatestMailLinksFor = async (
    request: APIRequestContext,
    email: string,
): Promise<string[]> => {
    const messageId = await findLatestMailIdFor(request, email);
    return messageId ? extractLinksFromMail(request, messageId) : [];
};
