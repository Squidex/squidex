/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input, Renderer2 } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { MarkdownDirective } from './markdown.directive';

@Component({
    template: '<div [inline]="inline()" [optional]="optional()" [sqxMarkdown]="markdown()"></div>',
    imports: [
        MarkdownDirective,
    ],
})
class TestHostComponent {
    public readonly markdown = input<string>('');
    public readonly inline = input(true);
    public readonly optional = input(false);
}

describe('MarkdownDirective', () => {
    let markdownFixture: ComponentFixture<TestHostComponent>;
    let setProperty: ReturnType<typeof vi.spyOn>;

    beforeEach(() => {
        markdownFixture = TestBed.createComponent(TestHostComponent);

        const renderer = markdownFixture.debugElement.query(By.css('div')).injector.get(Renderer2);

        setProperty = vi.spyOn(renderer, 'setProperty');
    });

    it('should render empty text as text', () => {
        setInputs({ markdown: '' });

        verifyTextRender('');
    });

    it('should render as text if result has no tags', () => {
        setInputs({ markdown: 'markdown', inline: true });

        verifyTextRender('markdown');
    });

    it('should render as text if optional', () => {
        setInputs({ markdown: '**bold**', optional: true });

        verifyTextRender('**bold**');
    });

    it('should render HTML as text if optional', () => {
        setInputs({ markdown: '<img src="x" onerror="alert(1)">', optional: true });

        verifyTextRender('<img src="x" onerror="alert(1)">');
    });

    it('should render HTML escaped if optional with exclamation', () => {
        setInputs({ markdown: '!<img src="x" onerror="alert(1)">', optional: true });

        verifyHtmlRender('&lt;img src=&quot;x&quot; onerror=&quot;alert(1)&quot;&gt;');
    });

    it('should render if optional with exclamation', () => {
        setInputs({ markdown: '!**bold**', optional: true });

        verifyHtmlRender('<strong>bold</strong>');
    });

    it('should render with exclamation if not optional', () => {
        setInputs({ markdown: '!text' });

        verifyTextRender('!text');
    });

    it('should render as HTML if it has encoded characters', () => {
        setInputs({ markdown: '\'Example\'', inline: false });

        verifyHtmlRender('<p>&#39;Example&#39;</p>\n');
    });

    it('should render as HTML if it has tags', () => {
        setInputs({ markdown: '**bold**', inline: false });

        verifyHtmlRender('<p><strong>bold</strong></p>\n');
    });

    it('should render as inline HTML if it has tags', () => {
        setInputs({ markdown: '**bold**' });

        verifyHtmlRender('<strong>bold</strong>');
    });

    it('should render HTML escaped', () => {
        setInputs({ markdown: '<h1>Header</h1>', inline: false });

        verifyHtmlRender('<p>&lt;h1&gt;Header&lt;/h1&gt;</p>\n');
    });

    it('should render again if markdown changes', () => {
        setInputs({ markdown: 'text1' });
        setInputs({ markdown: 'text2' });

        verifyTextRender('text2');
    });

    function setInputs(inputs: { markdown: string; inline?: boolean; optional?: boolean }) {
        for (const [key, value] of Object.entries(inputs)) {
            markdownFixture.componentRef.setInput(key, value);
        }

        markdownFixture.detectChanges();
    }

    function verifyTextRender(text: string) {
        expect(setProperty).toHaveBeenLastCalledWith(expect.anything(), 'textContent', text);
    }

    function verifyHtmlRender(text: string) {
        expect(setProperty).toHaveBeenLastCalledWith(expect.anything(), 'innerHTML', text);
    }
});
