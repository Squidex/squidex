import { h } from 'preact';
import { useEffect, useRef, useState } from 'preact/hooks';
import { IFrame } from './iframe';
import { Overlay } from './Overlay';

export const OverlayContainer = () => {
    const div = useRef<any>();
    const [target, setTarget] = useState<{ target: HTMLElement, token: string}>();
    const [targetUrl, setTargetUrl] = useState<string>();
    
    useEffect(() => {
        let previous: any;
        let previousTarget: HTMLElement | null = null;

        function listen(event: MouseEvent) {
            const target = event.target as HTMLElement;

            if (target && target !== previous) {
                try {
                    const token = target.getAttribute('squidex-token');
    
                    if (token) {
                        previousTarget = target;
    
                        
                        setTarget({ target, token });
                    } else if (previousTarget && !previousTarget.contains(target) && !div.current?.contains(target)) {
                        previousTarget = null;
    
                        setTarget(undefined);
                    }
                } catch {
                }
    
                previous = target;
            }
        }

        document.addEventListener('mousemove', listen);

        return () => {
            document.removeEventListener('mousemove', listen);
        }
    }, []);

    return (
        <div class='squidex' ref={div}>
            {target &&
                <Overlay onOpen={setTargetUrl} {...target} />
            }

            {targetUrl &&
                <IFrame url={targetUrl} onClose={() => setTargetUrl(undefined)} />
            }
        </div>
    );
}