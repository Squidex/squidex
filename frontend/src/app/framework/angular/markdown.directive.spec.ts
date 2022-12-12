/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Renderer2 } from '@angular/core';
import { IMock, It, Mock, Times } from 'typemoq';
import { MarkdownDirective } from './markdown.directive';

describe('MarkdownDirective', () => {
    let renderer: IMock<Renderer2>;
    let markdownElement = {};
    let markdownDirective: MarkdownDirective;

    beforeEach(() => {
        renderer = Mock.ofType<Renderer2>();

        markdownElement = {};
        markdownDirective = new MarkdownDirective(markdownElement as any, renderer.object);
    });

    it('should render empty text as text', () => {
        markdownDirective.markdown = '';
        markdownDirective.ngOnChanges();

        verifyTextRender('');
    });

    it('should render as text if result has no tags', () => {
        markdownDirective.inline = true;
        markdownDirective.markdown = 'markdown';
        markdownDirective.ngOnChanges();

        verifyTextRender('markdown');
    });

    it('should render as text if optional', () => {
        markdownDirective.optional = true;
        markdownDirective.markdown = '**bold**';
        markdownDirective.ngOnChanges();

        verifyTextRender('**bold**');
    });

    it('should render if optional with exclamation', () => {
        markdownDirective.optional = true;
        markdownDirective.markdown = '!**bold**';
        markdownDirective.ngOnChanges();

        verifyHtmlRender('<strong>bold</strong>');
    });

    it('should render as HTML if allowed', () => {
        markdownDirective.inline = false;
        markdownDirective.markdown = '**bold**';
        markdownDirective.ngOnChanges();

        verifyHtmlRender('<p><strong>bold</strong></p>\n');
    });

    it('should render as inline HTML if allowed', () => {
        markdownDirective.markdown = '!**bold**';
        markdownDirective.ngOnChanges();

        verifyHtmlRender('<strong>bold</strong>');
    });

    it('should render HTML escaped', () => {
        markdownDirective.inline = false;
        markdownDirective.markdown = '<h1>Header</h1>';
        markdownDirective.ngOnChanges();

        verifyHtmlRender('<p>&lt;h1&gt;Header&lt;/h1&gt;</p>\n');
    });

    function verifyTextRender(text: string) {
        renderer.verify(x => x.setProperty(It.isAny(), 'textContent', text), Times.once());

        expect().nothing();
    }

    function verifyHtmlRender(text: string) {
        renderer.verify(x => x.setProperty(It.isAny(), 'innerHTML', text), Times.once());

        expect().nothing();
    }
});