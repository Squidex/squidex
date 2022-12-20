/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { marked } from 'marked';

function renderLink(href: string, _: string, text: string) {
    if (href && href.startsWith('mailto')) {
        return text;
    } else {
        return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
    }
}

function renderInlineParagraph(text: string) {
    return text;
}

const RENDERER_DEFAULT = new marked.Renderer();
const RENDERER_INLINE = new marked.Renderer();

RENDERER_INLINE.paragraph = renderInlineParagraph;
RENDERER_INLINE.link = renderLink;
RENDERER_DEFAULT.link = renderLink;

export function renderMarkdown(input: string | undefined | null, inline: boolean) {
    if (!input) {
        return '';
    }

    input = escapeHTML(input);

    if (inline) {
        return marked(input, { renderer: RENDERER_INLINE });
    } else {
        return marked(input, { renderer: RENDERER_DEFAULT });
    }
}

const escapeTestNoEncode = /[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/;
const escapeReplaceNoEncode = new RegExp(escapeTestNoEncode.source, 'g');
const escapeReplacements = {
    '&' : '&amp;',
    '<' : '&lt;',
    '>' : '&gt;',
    '"' : '&quot;',
    '\'': '&#39;',
};

const getEscapeReplacement = (ch: string) => escapeReplacements[ch];

export function escapeHTML(html: string) {
    if (escapeTestNoEncode.test(html)) {
        return html.replace(escapeReplaceNoEncode, getEscapeReplacement);
    }

    return html;
}