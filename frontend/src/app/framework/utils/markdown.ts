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
        return `<a href="${href}">${text}</a>`;
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
    `.trim();
}

function renderInlineParagraph(text: string) {
    return text;
}

const RENDERER_INLINE = new marked.Renderer();
RENDERER_INLINE.paragraph = renderInlineParagraph;
RENDERER_INLINE.link = renderLink;
RENDERER_INLINE.code = renderCode;

const RENDERER_DEFAULT = new marked.Renderer();
RENDERER_DEFAULT.link = renderLink;
RENDERER_DEFAULT.code = renderCode;

export function markdownRender(input: string | undefined | null, inline: boolean, trusted = false) {
    if (!input) {
        return '';
    }

    if (!trusted) {
        input = escapeHTML(input);
    }

    if (inline) {
        return marked(input, { renderer: RENDERER_INLINE }) as string;
    } else {
        return marked(input, { renderer: RENDERER_DEFAULT }) as string;
    }
}

const ESCAPE_REPLACE_NO_ENCODE = /[<>"']|&(?!(#\d{1,7}|#[Xx][a-fA-F0-9]{1,6}|\w+);)/g;

const ESCAPE_REPLACEMENTS = {
    '&' : '&amp;',
    '<' : '&lt;',
    '>' : '&gt;',
    '"' : '&quot;',
    '\'': '&#39;',
} as Record<string, string>;

export function escapeHTML(html: string) {
    return html.replace(ESCAPE_REPLACE_NO_ENCODE, c => ESCAPE_REPLACEMENTS[c]);
}