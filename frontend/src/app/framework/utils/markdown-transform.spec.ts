/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { markdownExtractImage, markdownTransformImages } from './markdown-transform';

describe('MarkdownTransform', () => {
    it('should not extract image if markdown contains no image', () => {
        const md = '# Header';

        const result = markdownExtractImage(md);

        expect(result).toBeNull();
    });

    it('should not extract image if URL is not valid', () => {
        const md = '![](/image.png)';

        const result = markdownExtractImage(md);

        expect(result).toBeNull();
    });

    it('should extract image', () => {
        const md = '![](https://squidex.io/image.png)';

        const result = markdownExtractImage(md);

        expect(result).toEqual({ url: 'https://squidex.io/image.png', name: 'image.webp' });
    });

    it('should extract image with name', () => {
        const md = '![](https://squidex.io/image.png "My Picture")';

        const result = markdownExtractImage(md);

        expect(result).toEqual({ url: 'https://squidex.io/image.png', name: 'my-picture.webp' });
    });

    it('should extract image with lax name', () => {
        const md = '![](https://squidex.io/image.png Picture)';

        const result = markdownExtractImage(md);

        expect(result).toEqual({ url: 'https://squidex.io/image.png', name: 'picture.webp' });
    });

    it('should extract image with alt', () => {
        const md = '![Alt Text](https://squidex.io/image.png)';

        const result = markdownExtractImage(md);

        expect(result).toEqual({ url: 'https://squidex.io/image.png', name: 'alt-text.webp' });
    });

    it('should transform image url', async () => {
        const md = '![](https://squidex.io/image.png)';

        const result = await markdownTransformImages(md, img => Promise.resolve(`${img.url}?transformed`));

        expect(result).toEqual('![](https://squidex.io/image.png?transformed)');
    });

    it('should transform with name', async () => {
        const md = '![](https://squidex.io/image.png "My Picture")';

        const result = await markdownTransformImages(md, img => Promise.resolve(`${img.url}?transformed`));

        expect(result).toEqual('![](https://squidex.io/image.png?transformed "My Picture")');
    });

    it('should transform with lax name', async () => {
        const md = '![](https://squidex.io/image.png Picture)';

        const result = await markdownTransformImages(md, img => Promise.resolve(`${img.url}?transformed`));

        expect(result).toEqual('![](https://squidex.io/image.png?transformed "Picture")');
    });

    it('should transform with alt', async () => {
        const md = '![Alt](https://squidex.io/image.png)';

        const result = await markdownTransformImages(md, img => Promise.resolve(`${img.url}?transformed`));

        expect(result).toEqual('![Alt](https://squidex.io/image.png?transformed)');
    });

    it('should transform multiple images', async () => {
        const md = `
# Header 1

![Alt1](https://squidex.io/image1.png "Picture1")
![Alt2](https://squidex.io/image2.png "Picture2")

## Header 2
`;

        const result = await markdownTransformImages(md, img => Promise.resolve(`${img.url}?transformed`));

        expect(result).toEqual(`
# Header 1

![Alt1](https://squidex.io/image1.png?transformed "Picture1")
![Alt2](https://squidex.io/image2.png?transformed "Picture2")

## Header 2
`);
    });
});