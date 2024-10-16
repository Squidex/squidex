/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import fs from 'fs/promises';
import path from 'path';
import { v4 as uuidv4 } from 'uuid';
import { TEMPORARY_PATH } from '../playwright.config';

export function getRandomId() {
    return uuidv4().replace(/-/g, '');
}

export function escapeRegex(string: string) {
    const result = string.replace(/[/\-\\^$*+?.()|[\]{}]/g, '\\$&');

    return new RegExp(result);
}

export async function writeJsonAsync(name: string, json: any) {
    const fullPath = await getPath(name);

    await fs.writeFile(fullPath, JSON.stringify(json), { encoding: 'utf8' });
}

export async function readJsonAsync<T>(name: string, defaultValue: T) {
    const fullPath = await getPath(name);

    const json = await fs.readFile(fullPath, 'utf8');

    if (json) {
        return JSON.parse(json) as T;
    } else {
        return defaultValue;
    }
}

async function getPath(name: string) {
    const fullPath = path.join(TEMPORARY_PATH, `${name}.json`);

    await fs.mkdir(TEMPORARY_PATH, { recursive: true });

    return fullPath;
}