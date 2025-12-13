(function(){
    window.plotlyInterop = {
        _ensurePlotly: function(callback){
            if (window.Plotly){ callback(); return; }
            var s = document.createElement('script');
            s.src = 'https://cdn.plot.ly/plotly-2.24.1.min.js';
            s.onload = callback;
            s.onerror = function(){ console.error('Plotly load failed'); };
            document.head.appendChild(s);
        },
        plot: function(elementId, traces, layout){
            this._ensurePlotly(function(){
                try{
                    var el = document.getElementById(elementId);
                    if (!el) return;
                    Plotly.newPlot(el, traces, layout || {} );
                }catch(e){ console.error(e); }
            });
        },
        toast: function(message, isError){
            var containerId = 'toast-container';
            var container = document.getElementById(containerId);
            if (!container){
                container = document.createElement('div');
                container.id = containerId;
                container.style.position = 'fixed';
                container.style.top = '12px';
                container.style.right = '12px';
                container.style.zIndex = 10000;
                document.body.appendChild(container);
            }
            var toast = document.createElement('div');
            toast.className = 'plotly-toast';
            toast.style.marginBottom = '8px';
            toast.style.padding = '10px 14px';
            toast.style.borderRadius = '6px';
            toast.style.color = '#fff';
            toast.style.boxShadow = '0 2px 6px rgba(0,0,0,0.2)';
            toast.style.background = isError ? '#d9534f' : '#5cb85c';
            toast.textContent = message;
            container.appendChild(toast);
            setTimeout(function(){
                toast.style.transition = 'opacity 0.4s ease';
                toast.style.opacity = '0';
                setTimeout(function(){ container.removeChild(toast); }, 450);
            }, 3000);
        }
    };
})();
