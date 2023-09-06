/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { marked } from 'marked';
import { MathHelper } from './math-helper';

function renderLink(href: string, _: string, text: string) {
    if (href && href.startsWith('mailto')) {
        return text;
    } else {
        return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
    }
}

function renderCode(code: string) {
    const id = MathHelper.guid();

    return `
        <div class="code-container">
            <button type="button" class="code-copy" copy="${id}">
                <i class="icon-copy"></i>
            </button>

            <pre class="code" id="${id}">${code}</pre>
        </div>
    `;
}

function renderInlineParagraph(text: string) {
    return text;
}

const RENDERER_DEFAULT = new marked.Renderer();
const RENDERER_INLINE = new marked.Renderer();

RENDERER_INLINE.paragraph = renderInlineParagraph;
RENDERER_INLINE.link = renderLink;
RENDERER_INLINE.code = renderCode;
RENDERER_DEFAULT.link = renderLink;
RENDERER_DEFAULT.code = renderCode;

export function renderMarkdown(input: string | undefined | null, inline: boolean, trusted = false) {
    if (!input) {
        return '';
    }

    if (!trusted) {
        input = escapeHTML(input);
    }

    if (inline) {
        return marked(input, { renderer: RENDERER_INLINE, mangle: false, headerIds: false });
    } else {
        return marked(input, { renderer: RENDERER_DEFAULT, mangle: false, headerIds: false });
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
} as Record<string, string>;

const getEscapeReplacement = (ch: string) => escapeReplacements[ch];

export function escapeHTML(html: string) {
    if (escapeTestNoEncode.test(html)) {
        return html.replace(escapeReplaceNoEncode, getEscapeReplacement);
    }

    return html;
}