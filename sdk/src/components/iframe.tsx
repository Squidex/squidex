import { h } from 'preact';

export interface IFrameProps {
    // The url to embed.
    url: string;

    // When closed.
    onClose: () => void;
}

export const IFrame = (props: IFrameProps) => {
    const { url, onClose } = props;
    return (
        <div class='squidex-iframe'>
            <button class='squidex-iframe-close' onClick={onClose}>
                <svg version='1.1' xmlns='http://www.w3.org/2000/svg' width='18' height='18' viewBox='0 0 24 24'>
                    <path d='M18.984 6.422l-5.578 5.578 5.578 5.578-1.406 1.406-5.578-5.578-5.578 5.578-1.406-1.406 5.578-5.578-5.578-5.578 1.406-1.406 5.578 5.578 5.578-5.578z'></path>
                </svg>
            </button>

            <iframe src={url} frameBorder={0}></iframe>
        </div>
    );
};