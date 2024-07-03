/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import slugify from 'slugify';
import { MathHelper } from './math-helper';

const IMAGE_REGEX = /!\[(?<alt>[^\]]*)\]\((?<url>.*?)([\s]["\s]*(?<name>[^")]*)["\s]*)?\)/;
const IMAGES_REGEX = /!\[(?<alt>[^\]]*)\]\((?<url>.*?)([\s]["\s]*(?<name>[^")]*)["\s]*)?\)/g;

export type MarkdownImage = { url: string; name: string };

export function markdownHasImage(markdown: string) {
    return !!markdown && !!markdown.match(IMAGES_REGEX);
}

export function markdownExtractImage(markdown: string): MarkdownImage | null {
    if (!markdown) {
        return null;
    }

    const match = markdown.match(IMAGE_REGEX);

    if (!match?.groups) {
        return null;
    }

    const { url, alt, name } = match.groups as { url: string; alt?: string; name?: string };

    if (!isURL(url)) {
        return null;
    }

    return toImage({ url, alt, name });
}

export async function markdownTransformImages(markdown: string, replace: (image: MarkdownImage) => Promise<string>) {
    if (!markdown) {
        return markdown;
    }

    const jobs: { id: string; url: string; name?: string; alt?: string }[] = [];

    let transformed = markdown.replace(IMAGES_REGEX, (_, alt, url, _other, name) => {
        const id = MathHelper.guid();

        jobs.push({ id, url, name, alt });
        return id;
    });

    const promises = jobs.map(async job => {
        const url = await replace(toImage(job));

        return { job, url };
    });

    const results = await Promise.all(promises);

    for (const result of results) {
        const { job, url } = result;
        const name = job.name ? ` "${job.name}"` : '';

        transformed = transformed.replace(result.job.id, `![${job.alt || ''}](${url}${name})`);
    }

    return transformed;
}

const IMAGE_EXTENSIONS = ['.avif', '.jpeg', '.jpg', '.png', '.webp'];

function toImage(image: { url: string; name?: string; alt?: string }): MarkdownImage {
    let name = image.name || image.alt || 'image';

    name = slugify(name, { lower: true, trim: true });

    if (!IMAGE_EXTENSIONS.find(ex => name.endsWith(ex))) {
        name += '.webp';
    }

    return { url: image.url, name };
}

function isURL(input: string) {
    try {
        new URL(input);
        return true;
    } catch {
        return false;
    }
}