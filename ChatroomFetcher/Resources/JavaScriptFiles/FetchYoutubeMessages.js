(function() {
    let allMessageNodes = [].slice.apply(document.querySelectorAll('yt-live-chat-text-message-renderer'))
        .filter(node => !node.getAttribute('data-is-fetched'));

    function getMessage(node) {
        return {
            avatarImage: node.querySelector('yt-img-shadow img').getAttribute('src'),
            authorName: node.querySelector('#author-name').innerText,
            message: node.querySelector('#message').innerHTML
        };
    }

    let allData = allMessageNodes.map(node => getMessage(node));
    for (let node of allMessageNodes) node.setAttribute('data-is-fetched', 'true');
    return allData;
})();