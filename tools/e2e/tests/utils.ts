import fs from 'fs/promises';
import path from 'path';
import { TEMPORARY_PATH } from '../playwright.config';

export function getRandomId() {
    const id = new Date().getTime().toString();

    return id;
}

export async function writeJsonAsync(name: string, json: any) {
    const fullPath = await getPath(name);

    await fs.writeFile(fullPath, JSON.stringify(json), { encoding: 'utf8' });
}

export async function readJsonAsync<T>(name: string, defaultValue: T) {
    const fullPath = await getPath(name);

    const json = await fs.readFile(fullPath, 'utf8');

    if (json) {
        return JSON.parse(json);
    } else {
        return defaultValue;
    }
}

async function getPath(name: string) {
    const fullPath = path.join(TEMPORARY_PATH, `${name}.json`);

    await fs.mkdir(TEMPORARY_PATH, { recursive: true });

    return fullPath;
}