/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { marked, Renderer, Tokens } from 'marked';
import { MathHelper } from './math-helper';

const SAFE_SCHEME = /^(https?|mailto):/i;
const ANY_SCHEME = /^[^/?#]*[:&]/;

function isUnsafeUrl(url: string) {
    // Browsers ignore whitespace and control characters in the scheme, e.g. "java\tscript:".
    const normalized = Array.from(url).filter(c => c.charCodeAt(0) > 32).join('');

    if (SAFE_SCHEME.test(normalized)) {
        return false;
    }

    // Also treat entities as a scheme, because "javascript&#58;" is decoded by the browser.
    return ANY_SCHEME.test(normalized);
}

class CustomRenderer extends Renderer {
    public static readonly INSTANCE = new CustomRenderer();

    public link({ href, tokens }: Tokens.Link): string {
        const text = this.parser.parseInline(tokens);
        if (!href || isUnsafeUrl(href)) {
            return text;
        }

        if (href.startsWith('mailto')) {
            return `<a href="${href}">${text}</a>`;
        } else {
            return `<a href="${href}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
        }
    }

    public code({ text }: Tokens.Code): string {
        const id = MathHelper.guid();

        return `
            <div class="code-container">
                <button type="button" class="code-copy" copy="${id}">
                    <i class="icon-copy"></i>
                </button>
    
                <pre class="code" id="${id}">${text}</pre>
            </div>
        `.trim();
    }
}

class InlineRenderer extends CustomRenderer {
    public static readonly INSTANCE = new InlineRenderer();

    public paragraph({ tokens }: Tokens.Paragraph): string {
        return this.parser.parseInline(tokens);
    }
}

export function markdownRender(input: string | undefined | null, inline: boolean, trusted = false) {
    if (!input) {
        return '';
    }

    if (!trusted) {
        input = escapeHTML(input);
    }

    if (inline) {
        return marked(input, { renderer: InlineRenderer.INSTANCE }) as string;
    } else {
        return marked(input, { renderer: CustomRenderer.INSTANCE, breaks: true }) as string;
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